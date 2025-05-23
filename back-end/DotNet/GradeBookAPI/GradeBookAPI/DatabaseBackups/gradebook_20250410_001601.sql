PGDMP                  
        }        	   gradebook    15.3    15.3 �    �           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                      false            �           0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                      false            �           0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                      false            �           1262    16940 	   gradebook    DATABASE     �   CREATE DATABASE gradebook WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'English_United States.1252';
    DROP DATABASE gradebook;
                postgres    false            �            1259    25416    assignment_types    TABLE     ?  CREATE TABLE public.assignment_types (
    type_id integer NOT NULL,
    type_name character varying(50) NOT NULL,
    weight numeric(5,2) DEFAULT 100.00,
    description text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
 $   DROP TABLE public.assignment_types;
       public         heap    postgres    false            �            1259    25424    assignment_types_type_id_seq    SEQUENCE     �   CREATE SEQUENCE public.assignment_types_type_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public.assignment_types_type_id_seq;
       public          postgres    false    214            �           0    0    assignment_types_type_id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE public.assignment_types_type_id_seq OWNED BY public.assignment_types.type_id;
          public          postgres    false    215            �            1259    25425    assignments    TABLE     ,  CREATE TABLE public.assignments (
    assignment_id integer NOT NULL,
    class_id integer NOT NULL,
    type_id integer NOT NULL,
    title character varying(100) NOT NULL,
    description text,
    max_points numeric(7,2) NOT NULL,
    min_points numeric(7,2) DEFAULT 0,
    due_date date,
    is_published boolean DEFAULT false,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT assignments_min_points_check CHECK ((min_points >= (0)::numeric))
);
    DROP TABLE public.assignments;
       public         heap    postgres    false            �            1259    25435    assignments_assignment_id_seq    SEQUENCE     �   CREATE SEQUENCE public.assignments_assignment_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public.assignments_assignment_id_seq;
       public          postgres    false    216            �           0    0    assignments_assignment_id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE public.assignments_assignment_id_seq OWNED BY public.assignments.assignment_id;
          public          postgres    false    217            �            1259    25436 
   audit_logs    TABLE     =  CREATE TABLE public.audit_logs (
    log_id integer NOT NULL,
    user_id integer,
    action character varying(50) NOT NULL,
    entity_type character varying(50),
    entity_id integer,
    details jsonb,
    ip_address character varying(45),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
    DROP TABLE public.audit_logs;
       public         heap    postgres    false            �            1259    25442    audit_logs_log_id_seq    SEQUENCE     �   CREATE SEQUENCE public.audit_logs_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public.audit_logs_log_id_seq;
       public          postgres    false    218            �           0    0    audit_logs_log_id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE public.audit_logs_log_id_seq OWNED BY public.audit_logs.log_id;
          public          postgres    false    219            �            1259    25443    class_enrollments    TABLE     X  CREATE TABLE public.class_enrollments (
    enrollment_id integer NOT NULL,
    class_id integer NOT NULL,
    student_id integer NOT NULL,
    enrollment_date date DEFAULT CURRENT_DATE,
    status character varying(10) DEFAULT 'Active'::character varying,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT class_enrollments_status_check CHECK (((status)::text = ANY (ARRAY[('Active'::character varying)::text, ('Dropped'::character varying)::text, ('Completed'::character varying)::text])))
);
 %   DROP TABLE public.class_enrollments;
       public         heap    postgres    false            �            1259    25451 #   class_enrollments_enrollment_id_seq    SEQUENCE     �   CREATE SEQUENCE public.class_enrollments_enrollment_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 :   DROP SEQUENCE public.class_enrollments_enrollment_id_seq;
       public          postgres    false    220            �           0    0 #   class_enrollments_enrollment_id_seq    SEQUENCE OWNED BY     k   ALTER SEQUENCE public.class_enrollments_enrollment_id_seq OWNED BY public.class_enrollments.enrollment_id;
          public          postgres    false    221            �            1259    25452    classes    TABLE     �  CREATE TABLE public.classes (
    class_id integer NOT NULL,
    course_id integer NOT NULL,
    teacher_id integer NOT NULL,
    semester character varying(20) NOT NULL,
    academic_year character varying(10) NOT NULL,
    start_date date,
    end_date date,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
    DROP TABLE public.classes;
       public         heap    postgres    false            �            1259    25457    classes_class_id_seq    SEQUENCE     �   CREATE SEQUENCE public.classes_class_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 +   DROP SEQUENCE public.classes_class_id_seq;
       public          postgres    false    222            �           0    0    classes_class_id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE public.classes_class_id_seq OWNED BY public.classes.class_id;
          public          postgres    false    223            �            1259    25458    courses    TABLE     C  CREATE TABLE public.courses (
    course_id integer NOT NULL,
    course_name character varying(100) NOT NULL,
    course_code character varying(20) NOT NULL,
    description text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
    DROP TABLE public.courses;
       public         heap    postgres    false            �            1259    25465    courses_course_id_seq    SEQUENCE     �   CREATE SEQUENCE public.courses_course_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public.courses_course_id_seq;
       public          postgres    false    224            �           0    0    courses_course_id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE public.courses_course_id_seq OWNED BY public.courses.course_id;
          public          postgres    false    225            �            1259    25466    grade_history    TABLE     7  CREATE TABLE public.grade_history (
    history_id integer NOT NULL,
    grade_id integer NOT NULL,
    previous_points numeric(7,2) NOT NULL,
    new_points numeric(7,2) NOT NULL,
    changed_by integer NOT NULL,
    change_reason text,
    change_time timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
 !   DROP TABLE public.grade_history;
       public         heap    postgres    false            �            1259    25472    grade_history_history_id_seq    SEQUENCE     �   CREATE SEQUENCE public.grade_history_history_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public.grade_history_history_id_seq;
       public          postgres    false    226            �           0    0    grade_history_history_id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE public.grade_history_history_id_seq OWNED BY public.grade_history.history_id;
          public          postgres    false    227            �            1259    25473    grade_imports    TABLE     �  CREATE TABLE public.grade_imports (
    import_id integer NOT NULL,
    class_id integer NOT NULL,
    assignment_id integer,
    file_name character varying(255) NOT NULL,
    status character varying(20) DEFAULT 'Pending'::character varying NOT NULL,
    imported_by integer NOT NULL,
    result text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT grade_imports_status_check CHECK (((status)::text = ANY (ARRAY[('Pending'::character varying)::text, ('Processing'::character varying)::text, ('Completed'::character varying)::text, ('Failed'::character varying)::text])))
);
 !   DROP TABLE public.grade_imports;
       public         heap    postgres    false            �            1259    25482    grade_imports_import_id_seq    SEQUENCE     �   CREATE SEQUENCE public.grade_imports_import_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public.grade_imports_import_id_seq;
       public          postgres    false    228            �           0    0    grade_imports_import_id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE public.grade_imports_import_id_seq OWNED BY public.grade_imports.import_id;
          public          postgres    false    229            �            1259    25483    grades    TABLE     c  CREATE TABLE public.grades (
    grade_id integer NOT NULL,
    assignment_id integer NOT NULL,
    student_id integer NOT NULL,
    points numeric(7,2) NOT NULL,
    comment text,
    graded_by integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
    DROP TABLE public.grades;
       public         heap    postgres    false            �            1259    25490    grades_grade_id_seq    SEQUENCE     �   CREATE SEQUENCE public.grades_grade_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public.grades_grade_id_seq;
       public          postgres    false    230            �           0    0    grades_grade_id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE public.grades_grade_id_seq OWNED BY public.grades.grade_id;
          public          postgres    false    231            �            1259    25491    password_resets    TABLE     4  CREATE TABLE public.password_resets (
    reset_id integer NOT NULL,
    user_id integer NOT NULL,
    token character varying(100) NOT NULL,
    expires_at timestamp without time zone NOT NULL,
    used_at timestamp without time zone,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
 #   DROP TABLE public.password_resets;
       public         heap    postgres    false            �            1259    25495    password_resets_reset_id_seq    SEQUENCE     �   CREATE SEQUENCE public.password_resets_reset_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public.password_resets_reset_id_seq;
       public          postgres    false    232            �           0    0    password_resets_reset_id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE public.password_resets_reset_id_seq OWNED BY public.password_resets.reset_id;
          public          postgres    false    233            �            1259    25496    sessions    TABLE     A  CREATE TABLE public.sessions (
    session_id integer NOT NULL,
    user_id integer NOT NULL,
    token character varying(255) NOT NULL,
    ip_address character varying(45),
    user_agent text,
    expires_at timestamp without time zone NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
    DROP TABLE public.sessions;
       public         heap    postgres    false            �            1259    25502    sessions_session_id_seq    SEQUENCE     �   CREATE SEQUENCE public.sessions_session_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public.sessions_session_id_seq;
       public          postgres    false    234            �           0    0    sessions_session_id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE public.sessions_session_id_seq OWNED BY public.sessions.session_id;
          public          postgres    false    235            �            1259    25503    user_profiles    TABLE     �  CREATE TABLE public.user_profiles (
    profile_id integer NOT NULL,
    user_id integer NOT NULL,
    first_name character varying(50) NOT NULL,
    last_name character varying(50) NOT NULL,
    phone character varying(20),
    address character varying(255),
    profile_picture character varying(255),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
 !   DROP TABLE public.user_profiles;
       public         heap    postgres    false            �            1259    25510    user_profiles_profile_id_seq    SEQUENCE     �   CREATE SEQUENCE public.user_profiles_profile_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public.user_profiles_profile_id_seq;
       public          postgres    false    236            �           0    0    user_profiles_profile_id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE public.user_profiles_profile_id_seq OWNED BY public.user_profiles.profile_id;
          public          postgres    false    237            �            1259    25511    users    TABLE     �  CREATE TABLE public.users (
    user_id integer NOT NULL,
    username character varying(50) NOT NULL,
    email character varying(100) NOT NULL,
    password_hash character varying(255) NOT NULL,
    salt character varying(100) NOT NULL,
    role character varying(10) NOT NULL,
    last_login timestamp without time zone,
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);
    DROP TABLE public.users;
       public         heap    postgres    false            �            1259    25519    users_user_id_seq    SEQUENCE     �   CREATE SEQUENCE public.users_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public.users_user_id_seq;
       public          postgres    false    238            �           0    0    users_user_id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE public.users_user_id_seq OWNED BY public.users.user_id;
          public          postgres    false    239            �           2604    25520    assignment_types type_id    DEFAULT     �   ALTER TABLE ONLY public.assignment_types ALTER COLUMN type_id SET DEFAULT nextval('public.assignment_types_type_id_seq'::regclass);
 G   ALTER TABLE public.assignment_types ALTER COLUMN type_id DROP DEFAULT;
       public          postgres    false    215    214            �           2604    25521    assignments assignment_id    DEFAULT     �   ALTER TABLE ONLY public.assignments ALTER COLUMN assignment_id SET DEFAULT nextval('public.assignments_assignment_id_seq'::regclass);
 H   ALTER TABLE public.assignments ALTER COLUMN assignment_id DROP DEFAULT;
       public          postgres    false    217    216            �           2604    25522    audit_logs log_id    DEFAULT     v   ALTER TABLE ONLY public.audit_logs ALTER COLUMN log_id SET DEFAULT nextval('public.audit_logs_log_id_seq'::regclass);
 @   ALTER TABLE public.audit_logs ALTER COLUMN log_id DROP DEFAULT;
       public          postgres    false    219    218            �           2604    25523    class_enrollments enrollment_id    DEFAULT     �   ALTER TABLE ONLY public.class_enrollments ALTER COLUMN enrollment_id SET DEFAULT nextval('public.class_enrollments_enrollment_id_seq'::regclass);
 N   ALTER TABLE public.class_enrollments ALTER COLUMN enrollment_id DROP DEFAULT;
       public          postgres    false    221    220            �           2604    25524    classes class_id    DEFAULT     t   ALTER TABLE ONLY public.classes ALTER COLUMN class_id SET DEFAULT nextval('public.classes_class_id_seq'::regclass);
 ?   ALTER TABLE public.classes ALTER COLUMN class_id DROP DEFAULT;
       public          postgres    false    223    222            �           2604    25525    courses course_id    DEFAULT     v   ALTER TABLE ONLY public.courses ALTER COLUMN course_id SET DEFAULT nextval('public.courses_course_id_seq'::regclass);
 @   ALTER TABLE public.courses ALTER COLUMN course_id DROP DEFAULT;
       public          postgres    false    225    224            �           2604    25526    grade_history history_id    DEFAULT     �   ALTER TABLE ONLY public.grade_history ALTER COLUMN history_id SET DEFAULT nextval('public.grade_history_history_id_seq'::regclass);
 G   ALTER TABLE public.grade_history ALTER COLUMN history_id DROP DEFAULT;
       public          postgres    false    227    226            �           2604    25527    grade_imports import_id    DEFAULT     �   ALTER TABLE ONLY public.grade_imports ALTER COLUMN import_id SET DEFAULT nextval('public.grade_imports_import_id_seq'::regclass);
 F   ALTER TABLE public.grade_imports ALTER COLUMN import_id DROP DEFAULT;
       public          postgres    false    229    228            �           2604    25528    grades grade_id    DEFAULT     r   ALTER TABLE ONLY public.grades ALTER COLUMN grade_id SET DEFAULT nextval('public.grades_grade_id_seq'::regclass);
 >   ALTER TABLE public.grades ALTER COLUMN grade_id DROP DEFAULT;
       public          postgres    false    231    230            �           2604    25529    password_resets reset_id    DEFAULT     �   ALTER TABLE ONLY public.password_resets ALTER COLUMN reset_id SET DEFAULT nextval('public.password_resets_reset_id_seq'::regclass);
 G   ALTER TABLE public.password_resets ALTER COLUMN reset_id DROP DEFAULT;
       public          postgres    false    233    232            �           2604    25530    sessions session_id    DEFAULT     z   ALTER TABLE ONLY public.sessions ALTER COLUMN session_id SET DEFAULT nextval('public.sessions_session_id_seq'::regclass);
 B   ALTER TABLE public.sessions ALTER COLUMN session_id DROP DEFAULT;
       public          postgres    false    235    234            �           2604    25531    user_profiles profile_id    DEFAULT     �   ALTER TABLE ONLY public.user_profiles ALTER COLUMN profile_id SET DEFAULT nextval('public.user_profiles_profile_id_seq'::regclass);
 G   ALTER TABLE public.user_profiles ALTER COLUMN profile_id DROP DEFAULT;
       public          postgres    false    237    236            �           2604    25532    users user_id    DEFAULT     n   ALTER TABLE ONLY public.users ALTER COLUMN user_id SET DEFAULT nextval('public.users_user_id_seq'::regclass);
 <   ALTER TABLE public.users ALTER COLUMN user_id DROP DEFAULT;
       public          postgres    false    239    238            �          0    25416    assignment_types 
   TABLE DATA           k   COPY public.assignment_types (type_id, type_name, weight, description, created_at, updated_at) FROM stdin;
    public          postgres    false    214   i�       �          0    25425    assignments 
   TABLE DATA           �   COPY public.assignments (assignment_id, class_id, type_id, title, description, max_points, min_points, due_date, is_published, created_at, updated_at) FROM stdin;
    public          postgres    false    216   �       �          0    25436 
   audit_logs 
   TABLE DATA           v   COPY public.audit_logs (log_id, user_id, action, entity_type, entity_id, details, ip_address, created_at) FROM stdin;
    public          postgres    false    218   ��       �          0    25443    class_enrollments 
   TABLE DATA           �   COPY public.class_enrollments (enrollment_id, class_id, student_id, enrollment_date, status, created_at, updated_at) FROM stdin;
    public          postgres    false    220   ��       �          0    25452    classes 
   TABLE DATA           �   COPY public.classes (class_id, course_id, teacher_id, semester, academic_year, start_date, end_date, created_at, updated_at) FROM stdin;
    public          postgres    false    222   ߲       �          0    25458    courses 
   TABLE DATA           k   COPY public.courses (course_id, course_name, course_code, description, created_at, updated_at) FROM stdin;
    public          postgres    false    224   ;�       �          0    25466    grade_history 
   TABLE DATA           �   COPY public.grade_history (history_id, grade_id, previous_points, new_points, changed_by, change_reason, change_time) FROM stdin;
    public          postgres    false    226   ϳ       �          0    25473    grade_imports 
   TABLE DATA           �   COPY public.grade_imports (import_id, class_id, assignment_id, file_name, status, imported_by, result, created_at, updated_at) FROM stdin;
    public          postgres    false    228   �       �          0    25483    grades 
   TABLE DATA           y   COPY public.grades (grade_id, assignment_id, student_id, points, comment, graded_by, created_at, updated_at) FROM stdin;
    public          postgres    false    230   	�       �          0    25491    password_resets 
   TABLE DATA           d   COPY public.password_resets (reset_id, user_id, token, expires_at, used_at, created_at) FROM stdin;
    public          postgres    false    232   ]�       �          0    25496    sessions 
   TABLE DATA           n   COPY public.sessions (session_id, user_id, token, ip_address, user_agent, expires_at, created_at) FROM stdin;
    public          postgres    false    234   c�       �          0    25503    user_profiles 
   TABLE DATA           �   COPY public.user_profiles (profile_id, user_id, first_name, last_name, phone, address, profile_picture, created_at, updated_at) FROM stdin;
    public          postgres    false    236   ��       �          0    25511    users 
   TABLE DATA           �   COPY public.users (user_id, username, email, password_hash, salt, role, last_login, is_active, created_at, updated_at) FROM stdin;
    public          postgres    false    238   ˶       �           0    0    assignment_types_type_id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('public.assignment_types_type_id_seq', 1, true);
          public          postgres    false    215            �           0    0    assignments_assignment_id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('public.assignments_assignment_id_seq', 1, true);
          public          postgres    false    217            �           0    0    audit_logs_log_id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('public.audit_logs_log_id_seq', 52, true);
          public          postgres    false    219            �           0    0 #   class_enrollments_enrollment_id_seq    SEQUENCE SET     Q   SELECT pg_catalog.setval('public.class_enrollments_enrollment_id_seq', 1, true);
          public          postgres    false    221            �           0    0    classes_class_id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('public.classes_class_id_seq', 1, true);
          public          postgres    false    223            �           0    0    courses_course_id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('public.courses_course_id_seq', 1, true);
          public          postgres    false    225            �           0    0    grade_history_history_id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('public.grade_history_history_id_seq', 1, false);
          public          postgres    false    227            �           0    0    grade_imports_import_id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('public.grade_imports_import_id_seq', 1, false);
          public          postgres    false    229            �           0    0    grades_grade_id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('public.grades_grade_id_seq', 3, true);
          public          postgres    false    231            �           0    0    password_resets_reset_id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('public.password_resets_reset_id_seq', 3, true);
          public          postgres    false    233            �           0    0    sessions_session_id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('public.sessions_session_id_seq', 1, false);
          public          postgres    false    235            �           0    0    user_profiles_profile_id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('public.user_profiles_profile_id_seq', 9, true);
          public          postgres    false    237            �           0    0    users_user_id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('public.users_user_id_seq', 13, true);
          public          postgres    false    239            �           2606    25534 &   assignment_types assignment_types_pkey 
   CONSTRAINT     i   ALTER TABLE ONLY public.assignment_types
    ADD CONSTRAINT assignment_types_pkey PRIMARY KEY (type_id);
 P   ALTER TABLE ONLY public.assignment_types DROP CONSTRAINT assignment_types_pkey;
       public            postgres    false    214            �           2606    25536    assignments assignments_pkey 
   CONSTRAINT     e   ALTER TABLE ONLY public.assignments
    ADD CONSTRAINT assignments_pkey PRIMARY KEY (assignment_id);
 F   ALTER TABLE ONLY public.assignments DROP CONSTRAINT assignments_pkey;
       public            postgres    false    216            �           2606    25538    audit_logs audit_logs_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_pkey PRIMARY KEY (log_id);
 D   ALTER TABLE ONLY public.audit_logs DROP CONSTRAINT audit_logs_pkey;
       public            postgres    false    218            �           2606    25540 ;   class_enrollments class_enrollments_class_id_student_id_key 
   CONSTRAINT     �   ALTER TABLE ONLY public.class_enrollments
    ADD CONSTRAINT class_enrollments_class_id_student_id_key UNIQUE (class_id, student_id);
 e   ALTER TABLE ONLY public.class_enrollments DROP CONSTRAINT class_enrollments_class_id_student_id_key;
       public            postgres    false    220    220            �           2606    25542 (   class_enrollments class_enrollments_pkey 
   CONSTRAINT     q   ALTER TABLE ONLY public.class_enrollments
    ADD CONSTRAINT class_enrollments_pkey PRIMARY KEY (enrollment_id);
 R   ALTER TABLE ONLY public.class_enrollments DROP CONSTRAINT class_enrollments_pkey;
       public            postgres    false    220            �           2606    25544    classes classes_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY public.classes
    ADD CONSTRAINT classes_pkey PRIMARY KEY (class_id);
 >   ALTER TABLE ONLY public.classes DROP CONSTRAINT classes_pkey;
       public            postgres    false    222            �           2606    25546    courses courses_course_code_key 
   CONSTRAINT     a   ALTER TABLE ONLY public.courses
    ADD CONSTRAINT courses_course_code_key UNIQUE (course_code);
 I   ALTER TABLE ONLY public.courses DROP CONSTRAINT courses_course_code_key;
       public            postgres    false    224            �           2606    25548    courses courses_pkey 
   CONSTRAINT     Y   ALTER TABLE ONLY public.courses
    ADD CONSTRAINT courses_pkey PRIMARY KEY (course_id);
 >   ALTER TABLE ONLY public.courses DROP CONSTRAINT courses_pkey;
       public            postgres    false    224            �           2606    25550     grade_history grade_history_pkey 
   CONSTRAINT     f   ALTER TABLE ONLY public.grade_history
    ADD CONSTRAINT grade_history_pkey PRIMARY KEY (history_id);
 J   ALTER TABLE ONLY public.grade_history DROP CONSTRAINT grade_history_pkey;
       public            postgres    false    226            �           2606    25552     grade_imports grade_imports_pkey 
   CONSTRAINT     e   ALTER TABLE ONLY public.grade_imports
    ADD CONSTRAINT grade_imports_pkey PRIMARY KEY (import_id);
 J   ALTER TABLE ONLY public.grade_imports DROP CONSTRAINT grade_imports_pkey;
       public            postgres    false    228            �           2606    25554 *   grades grades_assignment_id_student_id_key 
   CONSTRAINT     z   ALTER TABLE ONLY public.grades
    ADD CONSTRAINT grades_assignment_id_student_id_key UNIQUE (assignment_id, student_id);
 T   ALTER TABLE ONLY public.grades DROP CONSTRAINT grades_assignment_id_student_id_key;
       public            postgres    false    230    230            �           2606    25556    grades grades_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY public.grades
    ADD CONSTRAINT grades_pkey PRIMARY KEY (grade_id);
 <   ALTER TABLE ONLY public.grades DROP CONSTRAINT grades_pkey;
       public            postgres    false    230            �           2606    25558 $   password_resets password_resets_pkey 
   CONSTRAINT     h   ALTER TABLE ONLY public.password_resets
    ADD CONSTRAINT password_resets_pkey PRIMARY KEY (reset_id);
 N   ALTER TABLE ONLY public.password_resets DROP CONSTRAINT password_resets_pkey;
       public            postgres    false    232            �           2606    25560    sessions sessions_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT sessions_pkey PRIMARY KEY (session_id);
 @   ALTER TABLE ONLY public.sessions DROP CONSTRAINT sessions_pkey;
       public            postgres    false    234            �           2606    25562    sessions sessions_token_key 
   CONSTRAINT     W   ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT sessions_token_key UNIQUE (token);
 E   ALTER TABLE ONLY public.sessions DROP CONSTRAINT sessions_token_key;
       public            postgres    false    234            �           2606    25564     user_profiles user_profiles_pkey 
   CONSTRAINT     f   ALTER TABLE ONLY public.user_profiles
    ADD CONSTRAINT user_profiles_pkey PRIMARY KEY (profile_id);
 J   ALTER TABLE ONLY public.user_profiles DROP CONSTRAINT user_profiles_pkey;
       public            postgres    false    236            �           2606    25566    users users_email_key 
   CONSTRAINT     Q   ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);
 ?   ALTER TABLE ONLY public.users DROP CONSTRAINT users_email_key;
       public            postgres    false    238            �           2606    25568    users users_pkey 
   CONSTRAINT     S   ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (user_id);
 :   ALTER TABLE ONLY public.users DROP CONSTRAINT users_pkey;
       public            postgres    false    238            �           2606    25570    users users_username_key 
   CONSTRAINT     W   ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);
 B   ALTER TABLE ONLY public.users DROP CONSTRAINT users_username_key;
       public            postgres    false    238            �           1259    25571    idx_assignments_class    INDEX     Q   CREATE INDEX idx_assignments_class ON public.assignments USING btree (class_id);
 )   DROP INDEX public.idx_assignments_class;
       public            postgres    false    216            �           1259    25572    idx_audit_logs_action    INDEX     N   CREATE INDEX idx_audit_logs_action ON public.audit_logs USING btree (action);
 )   DROP INDEX public.idx_audit_logs_action;
       public            postgres    false    218            �           1259    25573    idx_audit_logs_user    INDEX     M   CREATE INDEX idx_audit_logs_user ON public.audit_logs USING btree (user_id);
 '   DROP INDEX public.idx_audit_logs_user;
       public            postgres    false    218            �           1259    25574    idx_classes_teacher    INDEX     M   CREATE INDEX idx_classes_teacher ON public.classes USING btree (teacher_id);
 '   DROP INDEX public.idx_classes_teacher;
       public            postgres    false    222            �           1259    25575    idx_enrollments_class    INDEX     W   CREATE INDEX idx_enrollments_class ON public.class_enrollments USING btree (class_id);
 )   DROP INDEX public.idx_enrollments_class;
       public            postgres    false    220            �           1259    25576    idx_enrollments_student    INDEX     [   CREATE INDEX idx_enrollments_student ON public.class_enrollments USING btree (student_id);
 +   DROP INDEX public.idx_enrollments_student;
       public            postgres    false    220            �           1259    25577    idx_grades_assignment    INDEX     Q   CREATE INDEX idx_grades_assignment ON public.grades USING btree (assignment_id);
 )   DROP INDEX public.idx_grades_assignment;
       public            postgres    false    230            �           1259    25578    idx_grades_student    INDEX     K   CREATE INDEX idx_grades_student ON public.grades USING btree (student_id);
 &   DROP INDEX public.idx_grades_student;
       public            postgres    false    230            �           1259    25579    idx_users_role    INDEX     @   CREATE INDEX idx_users_role ON public.users USING btree (role);
 "   DROP INDEX public.idx_users_role;
       public            postgres    false    238            �           2606    25580 %   assignments assignments_class_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.assignments
    ADD CONSTRAINT assignments_class_id_fkey FOREIGN KEY (class_id) REFERENCES public.classes(class_id) ON DELETE CASCADE;
 O   ALTER TABLE ONLY public.assignments DROP CONSTRAINT assignments_class_id_fkey;
       public          postgres    false    222    216    3294            �           2606    25585 $   assignments assignments_type_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.assignments
    ADD CONSTRAINT assignments_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.assignment_types(type_id) ON DELETE CASCADE;
 N   ALTER TABLE ONLY public.assignments DROP CONSTRAINT assignments_type_id_fkey;
       public          postgres    false    3279    216    214            �           2606    25590 "   audit_logs audit_logs_user_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE SET NULL;
 L   ALTER TABLE ONLY public.audit_logs DROP CONSTRAINT audit_logs_user_id_fkey;
       public          postgres    false    3322    218    238                        2606    25595 1   class_enrollments class_enrollments_class_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.class_enrollments
    ADD CONSTRAINT class_enrollments_class_id_fkey FOREIGN KEY (class_id) REFERENCES public.classes(class_id) ON DELETE CASCADE;
 [   ALTER TABLE ONLY public.class_enrollments DROP CONSTRAINT class_enrollments_class_id_fkey;
       public          postgres    false    3294    220    222                       2606    25600 3   class_enrollments class_enrollments_student_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.class_enrollments
    ADD CONSTRAINT class_enrollments_student_id_fkey FOREIGN KEY (student_id) REFERENCES public.users(user_id) ON DELETE CASCADE;
 ]   ALTER TABLE ONLY public.class_enrollments DROP CONSTRAINT class_enrollments_student_id_fkey;
       public          postgres    false    220    238    3322                       2606    25605    classes classes_course_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.classes
    ADD CONSTRAINT classes_course_id_fkey FOREIGN KEY (course_id) REFERENCES public.courses(course_id) ON DELETE CASCADE;
 H   ALTER TABLE ONLY public.classes DROP CONSTRAINT classes_course_id_fkey;
       public          postgres    false    222    3299    224                       2606    25610    classes classes_teacher_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.classes
    ADD CONSTRAINT classes_teacher_id_fkey FOREIGN KEY (teacher_id) REFERENCES public.users(user_id) ON DELETE CASCADE;
 I   ALTER TABLE ONLY public.classes DROP CONSTRAINT classes_teacher_id_fkey;
       public          postgres    false    238    222    3322                       2606    25615 +   grade_history grade_history_changed_by_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.grade_history
    ADD CONSTRAINT grade_history_changed_by_fkey FOREIGN KEY (changed_by) REFERENCES public.users(user_id) ON DELETE CASCADE;
 U   ALTER TABLE ONLY public.grade_history DROP CONSTRAINT grade_history_changed_by_fkey;
       public          postgres    false    3322    226    238                       2606    25620 )   grade_history grade_history_grade_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.grade_history
    ADD CONSTRAINT grade_history_grade_id_fkey FOREIGN KEY (grade_id) REFERENCES public.grades(grade_id) ON DELETE CASCADE;
 S   ALTER TABLE ONLY public.grade_history DROP CONSTRAINT grade_history_grade_id_fkey;
       public          postgres    false    3307    226    230                       2606    25625 .   grade_imports grade_imports_assignment_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.grade_imports
    ADD CONSTRAINT grade_imports_assignment_id_fkey FOREIGN KEY (assignment_id) REFERENCES public.assignments(assignment_id) ON DELETE CASCADE;
 X   ALTER TABLE ONLY public.grade_imports DROP CONSTRAINT grade_imports_assignment_id_fkey;
       public          postgres    false    216    228    3281                       2606    25630 )   grade_imports grade_imports_class_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.grade_imports
    ADD CONSTRAINT grade_imports_class_id_fkey FOREIGN KEY (class_id) REFERENCES public.classes(class_id) ON DELETE CASCADE;
 S   ALTER TABLE ONLY public.grade_imports DROP CONSTRAINT grade_imports_class_id_fkey;
       public          postgres    false    222    3294    228                       2606    25635 ,   grade_imports grade_imports_imported_by_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.grade_imports
    ADD CONSTRAINT grade_imports_imported_by_fkey FOREIGN KEY (imported_by) REFERENCES public.users(user_id) ON DELETE CASCADE;
 V   ALTER TABLE ONLY public.grade_imports DROP CONSTRAINT grade_imports_imported_by_fkey;
       public          postgres    false    3322    228    238            	           2606    25640     grades grades_assignment_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.grades
    ADD CONSTRAINT grades_assignment_id_fkey FOREIGN KEY (assignment_id) REFERENCES public.assignments(assignment_id) ON DELETE CASCADE;
 J   ALTER TABLE ONLY public.grades DROP CONSTRAINT grades_assignment_id_fkey;
       public          postgres    false    216    3281    230            
           2606    25645    grades grades_graded_by_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.grades
    ADD CONSTRAINT grades_graded_by_fkey FOREIGN KEY (graded_by) REFERENCES public.users(user_id) ON DELETE CASCADE;
 F   ALTER TABLE ONLY public.grades DROP CONSTRAINT grades_graded_by_fkey;
       public          postgres    false    238    3322    230                       2606    25650    grades grades_student_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.grades
    ADD CONSTRAINT grades_student_id_fkey FOREIGN KEY (student_id) REFERENCES public.users(user_id) ON DELETE CASCADE;
 G   ALTER TABLE ONLY public.grades DROP CONSTRAINT grades_student_id_fkey;
       public          postgres    false    238    230    3322                       2606    25655 ,   password_resets password_resets_user_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.password_resets
    ADD CONSTRAINT password_resets_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE;
 V   ALTER TABLE ONLY public.password_resets DROP CONSTRAINT password_resets_user_id_fkey;
       public          postgres    false    238    232    3322                       2606    25660    sessions sessions_user_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT sessions_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE;
 H   ALTER TABLE ONLY public.sessions DROP CONSTRAINT sessions_user_id_fkey;
       public          postgres    false    234    3322    238                       2606    25665 (   user_profiles user_profiles_user_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.user_profiles
    ADD CONSTRAINT user_profiles_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE;
 R   ALTER TABLE ONLY public.user_profiles DROP CONSTRAINT user_profiles_user_id_fkey;
       public          postgres    false    3322    238    236            �   �   x�u��� �kx�{���c��2�32A�����.M���vg�feZ
)���f�`Rr��b����W8z��b��۲� 4Cޣ�AOO�	��l�����Z��wc���&O���`�,�B�lA5�V���j�S���F���H�9�      �      x�u�M� F�p�� df�`�m\z7X0i�P�9��{�%/��#E뎷�r��������5>�&d:c�%'8��׈ez�ؤB,E�9YE�Q������Eg����hp;��C��]{��j����'1      �   �  x��X]o�6}6��l@J�[����Vٚ�(T���ʒ'�i�a�}���8�c$��X���~^���������F���u#:��`�B(�� ��S�W�*r�Ϩnˢ��!�8��o��,˹�%�n2����n�C���8�m�i��d9��������Y�H�N��:�e �ni�U8��V��u� /�=�υ�H >�� ��
�?[�"#�	��#�G'�+�;i�]p���a|�U\�[��P��-��R	彋���#@$c��ͳ`��
"�!�F�.��8i�T�\fDeLX��[*��*�����*2m��^�!�+�ĝ��ͩ!�(����qU}pV,8Q&jg�����͸����G+���&ͅ!�QH&bt��pχǨ/
5 )H2�1vW6�!�q3M���}�x���,�p-3hx���Y
�x�)����Ic��F�x-HI�Lp���!ߵ�Й����-)�����ʍ����� �T�0)�@L��PR2I��Zs���rVmg�����6ߌ�8�I�澕n}��m����e9�|����Pܤ���a���\N�f�������W��5X�\
$���o��N��i#�����)x��q��M��ef���܈->�0�;��8.fF�"���N�����;<�mS/���	��5������ۘ�-"��0��>k��>E@��o+?�`���� �пɥ�N����X�.� �:<����_	��芩-Ň7E=w�?o���
�3�.�S�KI�`��{:XW8�Ȍ!NwO��jϩ�,�`����n���_�'�΅�*��|V���_����o|[Ó�����mbWx��������X��'_}Z��_�������f�<}{q|�!ǧ�C�ʪ(�lP�O�EB�%� ] 7�,@�o�><k�����a��D�n\��?�P��6�'>�	dZ�-dc�ܦTYf��SW�ghp�o{FY2��+����2/~@�{�yE��P�r��f=�-W
 ����1�
����"��XCY��׈��HE���-G����!aT4�hj�����2�&Y�����}���Cp�X�[��k�A2G�Zzt��rq-�I&�l_�LH2ʌ`H�0�4[!��a<�'�gK��H��3�Mq;�&�΅�P}������xN����X8�jҭ�d0l��Mx�vm��
(5����R���J��a w���<�~��v�T�1����B�ݻ<d�4�,$P(��A"�C��t���Y!�0����$t��=H�b鮣5L�I�L&��;��ڶK ��l��SuN3¥aZ#�_S����P����J�m\���bU�.��7_� 7��~#2�")��z��<m���S p�p�Ø$�N3hq��R�TA9�P"zCӌ��50�����P�f5[�y���i��=۱i&� a�U���HA��H�]�A�p�^o@Xa[�f�@�V
 �2e���I�=pn�BW!�/2�KJ      �   <   x�3�4�44�4202�50�5��tL.�,KEQ04�26�2��3�0201�)ei����� �p;      �   L   x�3�4�44�tK���4202�&�����!�ih�kh
b�� �X*�[�[���[�[���44����� �*      �   �   x�u�;�  �N�H JB:G�t�6+�$3|X
o�	|�S잨�9:rʰ �>"����)�ص��O�\N��B~�Ϳ
v��=�o�Vp\�
��>�r�5���'+�2v��(�ğ�s�G�,l      �      x������ � �      �      x������ � �      �   D   x�3�4�44�0�30�t��OQ(�/��44�4202�50�5�T0��24�22�315����#����� ��      �   �   x�e��N�@���S� s��2;�H0
CjI�M�bQB�O߅6���rr�@L~���j�P7q�_o�Vw�.��e�U%�r�/?^�� C�0� �@X�VrWR�u|Q�G53	0�M�vuV��GxKDT�\e������$��b�;�8��vѴ�X�.(#=3��2�*�z���R�PN ���9�+�&Q�d��8,�]��^�4�������p΀[�-*W��_~I
�SB��~RZ]����XX�      �      x������ � �      �   ;  x���OK�@�ϳ�b��efvg�]�K/����j5*��N��b#��e��7o_�r���8��~�F��eK�r����<�̢h"��Ǯ����%��+�����P�l<�cEq>G�?�ȷ�J^Ǖ�(�9�(F��(*��ȳD�I@t��f�5���"!�>��	v�I�I�vCo��n�c��C��y�+�R��B���1Z��Ag�G���Y6}{�N�b�T*f��)�<b�����Voݸ�Ý�S.��ۧ���w���{��_�v�k�C7�����/�x�U�E"��HSc�&�[�ߛ�3�|�ȴ      �   �  x�}�ko�Z�?��{�q]Y`�d#�BU�Z���MQEP�_�=��i�6��$/+<﬙�\�Q��ꤪ���I��,�CΕ�>�nwU]��q�T�V3U%ɖ�8��}��|�1��#�6��6�;i`�p:_��V¥�ưYX�]v5%참i?����g������P�#��/1跊�:Wէm��v�s��;��">�L��))�Lu��[�iK"�R0/�
�\��9�n�ک��f�
��=zؗ�4)�ʳ�-�ˡ[o��C˭��Xu>qK]����1����Iu�+7l����&����!�I.�iN���Ã�v��gɰd�ΰJ��l����m�<��:�n�H��������NG�B�Z����Z����-��(`��RG�.Av{y|�u��|4m�Q��d�΀<����IlT�n2!���R=r�h�fEy���3́�@�f{.�q}T$Gs3�"�h҇�^�	�H@ڑ^x_��M�{�pQ���j�	��+U���A�������n�:Sz��X���/�A���bB�^�l<8��q6��&�V%?3�> <�-2�N�@���7�P�J��̮aZ�+�8���rΩ�R��k�V��Ԧ��i�}���j��e~J�T�����L��1��z��$ٴ��?�I}Bx@ b�v$Ŷ��ҷ�:p�+����Nm���q??��Z�&�o��i�������4t�3o����0�R?�̦�k��`̤�u߄;�·дP=1�(��j{k�?Tg�Xt!��6 /A���#�Fd_����DB!�@܎�9N����y�?�إ��k[�"��h�F��c;�3�
'�J����)�������ER�N��Ў��u+G�st�w_��-��"��� u z[�/���ǋ�R�d=���e��"�LΌ]��6��h��ɿf*��^��'d%�^�&`�g�L� &ʶQ�R��@$�O���>#���E�O�6x&����j��/@ B�~v�Q��O	/	B���t�����     